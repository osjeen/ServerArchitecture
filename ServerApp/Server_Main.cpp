#include <iostream>
#include <boost/asio.hpp>
#include <thread>
#include <map>
#include <memory>
#include <mutex>

#define MAIN_PORT 5001

using boost::asio::ip::tcp;

class Session : public std::enable_shared_from_this<Session> {
public:
    tcp::socket socket_;
    int id_;

    Session(tcp::socket socket, int id) : socket_(std::move(socket)), id_(id) {}
};

class ServerApp {
    std::map<int, std::shared_ptr<Session>> clients;
    int next_client_id = 0;
    std::mutex clients_mutex;

public:
    void session_handler(tcp::socket socket) {
        int my_id;
        // 1. 세션 객체를 생성하여 스코프 밖에서도 소켓이 살게 함
        auto new_session = std::make_shared<Session>(std::move(socket), 0);

        {
            std::lock_guard<std::mutex> lock(clients_mutex);
            my_id = next_client_id++;
            new_session->id_ = my_id;
            clients[my_id] = new_session;
        }

        std::cout << "Client Connected. ID: " << my_id << std::endl;

        try {
            // 2. 무한 루프로 소켓이 닫히지 않게 유지
            for (;;) {
                char data[1024];
                boost::system::error_code error;
                
                // 여기서 에러가 난다면 클라이언트가 접속을 끊은 것임
                size_t length = new_session->socket_.read_some(boost::asio::buffer(data), error);

                if (error == boost::asio::error::eof) {
                    break; 
                } else if (error) {
                    throw boost::system::system_error(error);
                }

                boost::asio::write(new_session->socket_, boost::asio::buffer(data, length));
            }
        } catch (std::exception& e) {
            std::cerr << "ID " << my_id << " Error: " << e.what() << std::endl;
        }

        // 3. 종료 시 맵에서 제거
        {
            std::lock_guard<std::mutex> lock(clients_mutex);
            clients.erase(my_id);
            std::cout << "Client Disconnected. ID: " << my_id << std::endl;
        }
    }
};

int main() {
    ServerApp serverApp;
    boost::asio::io_context io_context;

    try {
        // 포트 재사용 옵션 적용
        tcp::acceptor acceptor(io_context);
        tcp::endpoint endpoint(tcp::v4(), MAIN_PORT);
        acceptor.open(endpoint.protocol());
        acceptor.set_option(tcp::acceptor::reuse_address(true));
        acceptor.bind(endpoint);
        acceptor.listen();

        std::cout << "Server is running on port " << MAIN_PORT << "..." << std::endl;

        for (;;) {
            // accept를 할 때마다 독립적인 소켓 객체 생성
            tcp::socket socket(io_context);
            acceptor.accept(socket);

            // 중요: 멤버 함수와 객체 주소를 정확히 전달
            std::thread(&ServerApp::session_handler, &serverApp, std::move(socket)).detach();
        }
    } catch (std::exception& e) {
        std::cerr << "Main Error: " << e.what() << std::endl;
    }

    return 0;
}