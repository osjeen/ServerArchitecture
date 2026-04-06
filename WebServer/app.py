from flask import Flask, jsonify, request, send_from_directory
from flask_cors import CORS
import time
import uuid
import threading
import os

app = Flask(__name__, static_folder=".")
CORS(app)

# 접속 유저 저장소 (실제 서비스에서는 Redis 등 사용 권장)
connected_users = {}
lock = threading.Lock()

IDLE_THRESHOLD = 60      # 60초 이상 활동 없으면 idle
AWAY_THRESHOLD = 300     # 300초 이상 활동 없으면 away
TIMEOUT_THRESHOLD = 600  # 600초 이상 응답 없으면 자동 제거


def get_status(last_activity):
    elapsed = time.time() - last_activity
    if elapsed < IDLE_THRESHOLD:
        return "online"
    elif elapsed < AWAY_THRESHOLD:
        return "idle"
    else:
        return "away"


def cleanup_inactive():
    """타임아웃된 유저 자동 제거 (백그라운드 스레드)"""
    while True:
        time.sleep(30)
        with lock:
            now = time.time()
            to_remove = [
                uid for uid, u in connected_users.items()
                if now - u["last_ping"] > TIMEOUT_THRESHOLD
            ]
            for uid in to_remove:
                del connected_users[uid]


threading.Thread(target=cleanup_inactive, daemon=True).start()


# ── API 엔드포인트 ─────────────────────────────────────────────

@app.route("/api/connect", methods=["POST"])
def connect():
    """클라이언트가 처음 접속할 때 호출. session_id 발급."""
    data = request.get_json(silent=True) or {}
    session_id = str(uuid.uuid4())
    ip = request.headers.get("X-Forwarded-For", request.remote_addr)
    now = time.time()

    with lock:
        connected_users[session_id] = {
            "id": session_id,
            "name": data.get("name", "익명 유저"),
            "ip": ip,
            "connected_at": now,
            "last_activity": now,
            "last_ping": now,
            "user_agent": request.headers.get("User-Agent", ""),
        }

    return jsonify({"session_id": session_id, "message": "연결되었습니다."}), 201


@app.route("/api/ping", methods=["POST"])
def ping():
    """클라이언트가 주기적으로 호출해서 살아있음을 알림."""
    data = request.get_json(silent=True) or {}
    session_id = data.get("session_id")

    with lock:
        if session_id not in connected_users:
            return jsonify({"error": "세션을 찾을 수 없습니다."}), 404
        now = time.time()
        connected_users[session_id]["last_ping"] = now
        if data.get("active"):
            connected_users[session_id]["last_activity"] = now

    return jsonify({"ok": True})


@app.route("/api/disconnect", methods=["POST"])
def disconnect():
    """클라이언트가 명시적으로 연결 해제."""
    data = request.get_json(silent=True) or {}
    session_id = data.get("session_id")

    with lock:
        connected_users.pop(session_id, None)

    return jsonify({"message": "연결이 해제되었습니다."})


@app.route("/api/users", methods=["GET"])
def list_users():
    """현재 접속 중인 유저 목록 반환."""
    with lock:
        users = []
        for u in connected_users.values():
            users.append({
                "id": u["id"],
                "name": u["name"],
                "ip": u["ip"],
                "connected_at": u["connected_at"],
                "last_activity": u["last_activity"],
                "status": get_status(u["last_activity"]),
                "duration_seconds": int(time.time() - u["connected_at"]),
            })

    # 접속 시간 최신순 정렬
    users.sort(key=lambda x: x["connected_at"], reverse=True)

    return jsonify({
        "total": len(users),
        "online": sum(1 for u in users if u["status"] == "online"),
        "idle": sum(1 for u in users if u["status"] == "idle"),
        "away": sum(1 for u in users if u["status"] == "away"),
        "users": users,
    })


@app.route("/api/kick/<session_id>", methods=["DELETE"])
def kick_user(session_id):
    """관리자가 특정 유저 강제 연결 해제."""
    with lock:
        user = connected_users.pop(session_id, None)

    if not user:
        return jsonify({"error": "유저를 찾을 수 없습니다."}), 404

    return jsonify({"message": f"{user['name']} 연결이 해제되었습니다."})


@app.route("/")
def index():
    return send_from_directory(".", "index.html")


if __name__ == "__main__":
    print("Flask 서버 시작: http://localhost:8080")
    app.run(debug=True, host="0.0.0.0", port=8080)
