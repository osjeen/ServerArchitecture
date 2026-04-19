#include <iostream>
#include <boost/asio.hpp>
#include <thread>
#include <map>
#include <memory>
#include <mutex>
#include"ClientStructure.hpp"
#include <boost/uuid/uuid.hpp>            // 기본 객체
#include <boost/uuid/uuid_generators.hpp> // 생성기 (random_generator)
#include <boost/uuid/uuid_io.hpp>         // 문자열 변환 (to_string)
#include <boost/container_hash/hash.hpp>


#define MAIN_PORT 5001

using boost::asio::ip::tcp;

using json = nlohmann::json;

class SharedClientManager {
public:
    void add_client(const ClientStructure& client) {
        std::lock_guard<std::mutex> lock(mutex_);
        data_.client_structures.push_back(client);
    }

    std::string get_json_data() {
        std::lock_guard<std::mutex> lock(mutex_);
        // {"clients": [...]} 형태로 출력됨
        return nlohmann::json(data_).dump();
    }

private:
    ClientMap data_; // 껍데기 구조체를 멤버로 보유
    std::mutex mutex_;
};

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
    void session_handler(SharedClientManager* scm,tcp::socket socket) {
        int my_id;
        // 1. 세션 객체를 생성하여 스코프 밖에서도 소켓이 살게 함
        auto new_session = std::make_shared<Session>(std::move(socket), 0);

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
                
                //Server Res
                std::cout << "[SUCCESS] Received length: " << length << std::endl;

                int query_id=(int)(unsigned char)data[0];
                if (length > 0) {
                    std::cout << "[DATA] First Byte (ID): " << query_id<< std::endl;
                }
                if(query_id==1){
                    //extract path data
                    std::string path(data+ 1,length-1);

                    //gen uuid
                    // 1. UUID 생성
                    boost::uuids::random_generator gen;
                    boost::uuids::uuid u = gen();

                    // 2. UUID를 int(또는 size_t)로 변환 (해시 이용)
                    size_t uuid_hash = boost::hash<boost::uuids::uuid>{}(u);
                    int final_uuid = static_cast<int>(uuid_hash);

                    //Assign Client
                    ClientStructure newClient;
                    newClient.path=path;
                    newClient.uuid=final_uuid;
                    newClient.ip=new_session->socket_.remote_endpoint().address().to_string();
                    scm->add_client(newClient);


                    //Send Client ID
                    std::cout<<"ID:"<<newClient.uuid<<std::endl;
                    unsigned char bytes[4];
                    std::memcpy(bytes, &newClient.uuid, sizeof(int));
                    boost::asio::write(new_session->socket_, boost::asio::buffer(bytes, length));
                }
                if(query_id==2){
                    //Extract path

                    //Get Clients in path
                    std::string json = scm->get_json_data();
                    std::cout<<json<<std::endl;
                    uint32_t host_len = static_cast<uint32_t>(json.size());

                    uint32_t net_len = boost::asio::detail::socket_ops::host_to_network_long(host_len);

                    std::vector<boost::asio::const_buffer> buffers;
                    buffers.push_back(boost::asio::buffer(&net_len, sizeof(net_len))); // net_len 사용
                    buffers.push_back(boost::asio::buffer(json));                     // json 사용

                    boost::system::error_code ec;

                    boost::asio::write(new_session->socket_, buffers, ec);

                    if (ec) {
                        std::cerr << "전송 에러: " << ec.message() << std::endl;
                    } else {
                        std::cout << "성공적으로 " << host_len << " 바이트 전송함." << std::endl;
                    }
                }
                else{
                    boost::asio::write(new_session->socket_, boost::asio::buffer(data, length));
                }
            }
        } catch (std::exception& e) {
            std::cerr << "ID " << my_id << " Error: " << e.what() << std::endl;
        }

        // 3. 종료 시 맵에서 제거
        {
            std::lock_guard<std::mutex> lock(clients_mutex);
            clients.erase(my_id);
        }
    }
};

int main() {
    ServerApp serverApp;
    SharedClientManager scm;
    boost::asio::io_context io_context;

    try {
        // 포트 재사용 옵션 적용
        tcp::acceptor acceptor(io_context);
        tcp::endpoint endpoint(tcp::v4(), MAIN_PORT);
        acceptor.open(endpoint.protocol());
        acceptor.set_option(tcp::acceptor::reuse_address(true));
        acceptor.bind(endpoint);
        acceptor.listen();

        std::cout << "Unity Server is running on port " << MAIN_PORT << "..." << std::endl;

        for (;;) {
            // accept를 할 때마다 독립적인 소켓 객체 생성
            tcp::socket socket(io_context);
            acceptor.accept(socket);

            // 중요: 멤버 함수와 객체 주소를 정확히 전달
            std::thread(&ServerApp::session_handler, &serverApp,&scm,std::move(socket)).detach();
        }
    } catch (std::exception& e) {
        std::cerr << "Main Error: " << e.what() << std::endl;
    }

    return 0;
}