cmake_minimum_required(VERSION 3.10)
project(UnityTcpServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 1. Homebrew 경로 설정 (혹시 모르니 유지)
if(APPLE)
    list(APPEND CMAKE_PREFIX_PATH "/opt/homebrew")
endif()

# 2. 패키지 찾기 
# [중요] system은 이제 헤더 전용이라 여기서 뺍니다. thread만 찾으세요.
find_package(nlohmann_json REQUIRED)
find_package(Boost REQUIRED COMPONENTS thread) 

# 3. 실행 파일 생성
add_executable(tcp_server Server_Main.cpp)

# 4. 라이브러리 연결
# Boost::system 타겟은 하위 호환성을 위해 남겨둬도 되지만, 
# 이제는 사실상 아무것도 링크하지 않는 빈 껍데기입니다.
target_link_libraries(tcp_server 
    PRIVATE 
        nlohmann_json::nlohmann_json
        Boost::thread
)

target_include_directories(tcp_server PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})