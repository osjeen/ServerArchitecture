cmake_minimum_required(VERSION 3.10)
project(BoostTcpExample)

# C++ 표준 설정
set(CMAKE_CXX_STANDARD 17)

# Boost 패키지 찾기 (Asio는 헤더 전용이지만, 시스템 라이브러리가 필요할 수 있음)
find_package(Boost REQUIRED)

if(Boost_FOUND)
    include_directories(${Boost_INCLUDE_DIRS})
    add_executable(tcp_server Server_Main.cpp)
    target_link_libraries(tcp_server ${Boost_LIBRARIES})
endif()