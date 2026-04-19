#include <iostream>
#include <vector>
#include <string>
#include <boost/asio.hpp>
#include <nlohmann/json.hpp>
#include <string>

using json = nlohmann::json;

struct ClientStructure {
    // endpoint 대신 통신에 필요한 정보만 문자열과 숫자로 저장
    std::string ip;
    int uuid;
    std::string path;

    // 매크로: 구조체 내부 변수 이름과 정확히 일치해야 함
    NLOHMANN_DEFINE_TYPE_INTRUSIVE(ClientStructure, ip, uuid, path)
};

// 2. ClientMap 정의
struct ClientMap {
    public:
        std::vector<ClientStructure> client_structures;
    NLOHMANN_DEFINE_TYPE_INTRUSIVE(ClientMap, client_structures)
};