FROM golang:1.23.1 AS builder

WORKDIR /app
ENV GO111MODULE=on

COPY ../../src/Guestbooky-backup/ .

RUN go mod download

RUN go build -o guestbooky-backup cmd/guestbooky-backup/guestbooky-backup.go

FROM alpine:latest  

WORKDIR /root

COPY --from=builder /app/guestbooky-backup .
RUN chmod +x /root/guestbooky-backup

CMD ["./guestbooky-backup"]