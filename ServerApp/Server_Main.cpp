#include <iostream>
#include <boost/asio.hpp>
#include <thread>
#include <map>
#include <memory>
#include <mutex> // 스레드 안전을 위해 추가

#define MAIN_PORT 5001

using boost::asio::ip::tcp;

class Session : public std::enable_shared_from_this<Session> {
public:
    tcp::socket socket_;
    std::string user_id;

    Session(tcp::socket socket) : socket_(std::move(socket)) {}
};

class ServerApp {
    std::map<int, std::shared_ptr<Session>> clients;
    int next_client_id = 0;
    std::mutex clients_mutex; // 여러 스레드가 map을 동시에 건드리지 못하게 보호

public:
    // 멤버 함수를 스레드에서 돌리려면 인스턴스 포인터가 필요합니다.
    void session_handler(tcp::socket socket) {
        auto new_session = std::make_shared<Session>(std::move(socket));
        
        // Lock 가드: 이 블록 안에서만 map 접근을 허용
        {
            std::lock_guard<std::mutex> lock(clients_mutex);
            clients[next_client_id++] = new_session;
            std::cout << "Client connected. ID: " << next_client_id - 1 << std::endl;
        }

        // 이후 데이터 송수신 로직(read/write)을 여기에 추가하면 됩니다.
    }
};

int main() {
    ServerApp serverApp;
    
    try {
        boost::asio::io_context io_context;
        tcp::acceptor acceptor(io_context);
        tcp::endpoint endpoint(tcp::v4(), MAIN_PORT);

        acceptor.open(endpoint.protocol());
        // 이 옵션을 추가하면 "Address already in use" 에러를 방지할 수 있습니다.
        acceptor.set_option(tcp::acceptor::reuse_address(true)); 
        acceptor.bind(endpoint);
        acceptor.listen();
        std::cout << "멀티스레드 서버 대기 중 (Port: " << MAIN_PORT << ")..." << std::endl;

        for (;;) {
            tcp::socket socket(io_context);
            acceptor.accept(socket);

            // 수정된 부분: 멤버 함수 호출 시 &ServerApp::session_handler와 객체 주소(&serverApp)를 전달해야 함
            std::thread(&ServerApp::session_handler, &serverApp, std::move(socket)).detach();
        }
    } catch (std::exception& e) {
        std::cerr << "Server Error: " << e.what() << "\n";
    }
    return 0;
}