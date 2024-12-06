package main

import (
	"fmt"
	"os"

	"github.com/cotti/Guestbooky-backup/internal/compactor"
)

func main() {
	err := compactor.Compact(os.Args[1], os.Args[2])
	if err != nil {
		fmt.Println("An error occurred while compacting:", err.Error())
		os.Exit(1)
	}
	os.Exit(0)
}
