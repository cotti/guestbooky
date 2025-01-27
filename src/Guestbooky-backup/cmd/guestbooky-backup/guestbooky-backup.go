package main

import (
	"fmt"
	"os"

	"github.com/cotti/Guestbooky-backup/internal/compactor"
	"github.com/cotti/Guestbooky-backup/internal/uploader"
)

func main() {
	sourceFile := os.Getenv("BACKUP_SOURCE_PATH")
	destinationFile := os.Getenv("BACKUP_DESTINATION_PATH")
	err := compactor.Compact(sourceFile, destinationFile)
	if err != nil {
		fmt.Println("An error occurred while compacting:", err.Error())
		os.Exit(1)
	}

	err = uploader.Upload(destinationFile)
	if err != nil {
		fmt.Println("An error occurred while uploading:", err.Error())
		os.Exit(1)
	}

	err = os.Remove("/backups/backups_done")
	if err != nil {
		fmt.Println("An error occurred while removing the file:", err.Error())
		os.Exit(1)
	}

	os.Exit(0)
}
