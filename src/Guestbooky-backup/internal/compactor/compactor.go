package compactor

import (
	"compress/gzip"
	"errors"
	"fmt"
	"io"
	"io/fs"
	"os"
)

func Compact(source, destination string) error {
	fmt.Println(
		"Compacting", source, "to", destination,
	)

	if _, err := os.Stat(source); errors.Is(err, fs.ErrNotExist) {
		return errors.New("source file does not exist")
	}

	originFileHandle, err := os.Open(source)
	if err != nil {
		return errors.New("failed to open source file")
	}
	defer originFileHandle.Close()

	destinationFileHandle, err := os.Create(destination)
	if err != nil {
		return errors.New("failed to create destination file")
	}
	defer destinationFileHandle.Close()

	zipWriter := gzip.NewWriter(destinationFileHandle)
	defer zipWriter.Close()

	if _, err := io.Copy(zipWriter, originFileHandle); err != nil {
		return errors.New("failed to copy file")
	}

	return nil
}
