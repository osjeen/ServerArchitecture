#include <iostream>
#include <boost/asio.hpp>
#include <thread>
#include <memory>

#define SERVER_ADR "127.0.0.1"
#define MAIN_PORT 5001
#define UNITY_PORT 5004

using boost::asio::ip::tcp;

// 서버 응답 -> 유니티 전달 함수
void relay_server_to_internal(std::shared_ptr<tcp::socket> server_sock, std::shared_ptr<tcp::socket> internal_sock) {
    try {
        char data[4096];
        for (;;) {
            boost::system::error_code error;
            size_t length = server_sock->read_some(boost::asio::buffer(data), error);
            
            if (error) break; // 메인 서버 연결 끊기면 종료

            // 유니티 소켓이 살아있는지 확인 후 전송
            if (internal_sock->is_open()) {
                boost::asio::write(*internal_sock, boost::asio::buffer(data, length));
            } else {
                break;
            }
        }
    } catch (...) {}
    std::cout << "중계 스레드 종료." << std::endl;
}

int main() {
    try {
        boost::asio::io_context io_context;

        // 1. 내부 앱 대기 설정
        tcp::acceptor acceptor(io_context, tcp::endpoint(tcp::v4(), UNITY_PORT));
        acceptor.set_option(tcp::acceptor::reuse_address(true));

        for (;;) {
            std::cout << "\n[대기 중] 유니티 접속을 기다립니다..." << std::endl;
            
            auto internal_sock = std::make_shared<tcp::socket>(io_context);
            acceptor.accept(*internal_sock);
            std::cout << "[연결됨] 유니티 클라이언트 접속." << std::endl;

            // 2. 유니티가 접속할 때마다 메인 서버와도 새로 연결 (가장 확실한 방법)
            auto server_sock = std::make_shared<tcp::socket>(io_context);
            boost::system::error_code ec;
            server_sock->connect(tcp::endpoint(boost::asio::ip::make_address(SERVER_ADR), MAIN_PORT), ec);

            if (ec) {
                std::cerr << "메인 서버 연결 실패: " << ec.message() << std::endl;
                internal_sock->close();
                continue;
            }
            std::cout << "[연결됨] 메인 서버와 연결 완료." << std::endl;

            // 3. 중계 스레드 실행
            std::thread t(relay_server_to_internal, server_sock, internal_sock);
            t.detach(); 

            // 4. 메인 루프 (유니티 -> 서버)
            try {
                char data[4096];
                for (;;) {
                    boost::system::error_code error;
                    size_t length = internal_sock->read_some(boost::asio::buffer(data), error);
                    
                    if (error == boost::asio::error::eof) {
                        std::cout << "[종료] 유니티가 연결을 끊었습니다." << std::endl;
                        break;
                    } else if (error) {
                        throw boost::system::system_error(error);
                    }

                    // 서버로 데이터 전달
                    boost::asio::write(*server_sock, boost::asio::buffer(data, length));
                }
            } catch (std::exception& e) {
                std::cerr << "통신 중 에러: " << e.what() << std::endl;
            }

            // 소켓 정리 (유니티가 나갔으므로 둘 다 닫음)
            internal_sock->close();
            server_sock->close();
        }

    } catch (std::exception& e) {
        std::cerr << "릴레이 서버 치명적 에러: " << e.what() << std::endl;
    }
    return 0;
}