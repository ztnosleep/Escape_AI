# Sử dụng Ubuntu 22.04 làm Base Image
FROM ubuntu:22.04

# Tránh các câu hỏi interactive khi cài đặt
ENV DEBIAN_FRONTEND=noninteractive

# Cập nhật và cài đặt các thư viện cần thiết cho Unity Server
RUN apt-get update && apt-get install -y \
    ca-certificates \
    openssl \
    # Thêm thư viện 32-bit/64-bit cơ bản phòng trường hợp cần thiết
    lib32gcc-s1 \
    libsdl2-2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Tạo user 'unity' (Best Practice về bảo mật)
RUN useradd -m -d /home/unity unity

# Thiết lập thư mục làm việc (Mọi lệnh sau sẽ chạy trong đây)
WORKDIR /home/unity/server

# Copy toàn bộ file đã build từ máy bạn vào Docker Image
# DÒNG NÀY RẤT QUAN TRỌNG: Phải khớp với tên thư mục bạn đã build.
COPY --chown=unity:unity ./Builds/Server/ .

# Chuyển sang user unity
USER unity

# Cấp quyền thực thi cho file game
RUN chmod +x ./MyGameServer.x86_64

# Mở Port (Ví dụ port 7777 UDP)
EXPOSE 7777/udp

# Lệnh chạy Server khi container khởi động
# -batchmode và -nographics để đảm bảo chạy không đồ họa
ENTRYPOINT ["./MyGameServer.x86_64", "-batchmode", "-nographics", "-logFile", "-"]