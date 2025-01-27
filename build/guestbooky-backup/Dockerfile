FROM golang:1.23.1 AS builder

WORKDIR /app
ENV GO111MODULE=on

COPY ../../src/Guestbooky-backup/ .

RUN go mod download

RUN go build -o guestbooky-backup cmd/guestbooky-backup/guestbooky-backup.go

# Start a new stage from scratch
FROM alpine:latest  

WORKDIR /root/

# Copy the Pre-built binary file from the previous stage
COPY --from=builder /app/guestbooky-backup .

# Command to run the executable
CMD ["./guestbooky-backup"]