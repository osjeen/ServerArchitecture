#include <iostream>
#include <boost/asio.hpp>
#include <thread>
#define SERVER_ADR "127.0.0.1"
#define MAIN_PORT 5001
#define UNITY_PORT 5004
using boost::asio::ip::tcp;

// 메인 서버로부터 오는 답변을 읽어서 내부 앱에 전달하는 스레드
void relay_server_to_internal(std::shared_ptr<tcp::socket> server_sock, std::shared_ptr<tcp::socket> internal_sock) {
    try {
        char data[1024];
        for (;;) {
            boost::system::error_code error;
            size_t length = server_sock->read_some(boost::asio::buffer(data), error);
            if (error) break;

            // 서버 응답 -> 내부 앱으로 전달
            boost::asio::write(*internal_sock, boost::asio::buffer(data, length));

            int d;
            std::cin>>d;
        }
    } catch (...) {}
}

int main() {
    try {
        boost::asio::io_context io_context;

        // 1. 메인 서버(5000)에 먼저 접속
        auto server_sock = std::make_shared<tcp::socket>(io_context);
        server_sock->connect(tcp::endpoint(boost::asio::ip::make_address(SERVER_ADR), MAIN_PORT));
        std::cout << "메인 서버 연결 완료." << std::endl;

        // 2. 내부 앱(5004) 접속 대기
        tcp::acceptor acceptor(io_context, tcp::endpoint(tcp::v4(), UNITY_PORT));
        acceptor.set_option(tcp::acceptor::reuse_address(true));
        std::cout << "내부 앱 대기 중 (Port: 5004)..." << std::endl;

        auto internal_sock = std::make_shared<tcp::socket>(io_context);
        acceptor.accept(*internal_sock);
        std::cout << "내부 앱 연결됨." << std::endl;

        // 3. 서버 -> 내부 앱 전달 스레드 실행
        std::thread(relay_server_to_internal, server_sock, internal_sock).detach();

        // 4. 내부 앱 -> 서버 전달 (메인 루프)
        char data[1024];
        for (;;) {
            boost::system::error_code error;
            size_t length = internal_sock->read_some(boost::asio::buffer(data), error);
            if (error == boost::asio::error::eof) break;

            // 내부 앱 요청 -> 메인 서버로 전달
            boost::asio::write(*server_sock, boost::asio::buffer(data, length));
            
            int d;
            std::cin>>d;
        }

    } catch (std::exception& e) {
        std::cerr << "중계기 에러: " << e.what() << std::endl;
    }
    return 0;
}