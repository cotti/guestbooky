package compactor

import (
	"archive/tar"
	"compress/gzip"
	"errors"
	"fmt"
	"io"
	"io/fs"
	"os"
	"path/filepath"
)

func Compact(source, destination string) error {
	fmt.Println(
		"Compacting", source, "to", destination,
	)

	if _, err := os.Stat(source); errors.Is(err, fs.ErrNotExist) {
		return errors.New("source does not exist")
	}

	destinationFileHandle, err := os.Create(destination)
	if err != nil {
		return errors.New("failed to create destination file")
	}
	defer destinationFileHandle.Close()

	zipWriter := gzip.NewWriter(destinationFileHandle)
	defer zipWriter.Close()

	err = filepath.Walk(source, func(path string, info fs.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if info.IsDir() {
			return nil
		}

		originFileHandle, err := os.Open(path)
		if err != nil {
			return errors.New("failed to open origin file: " + err.Error())
		}
		defer originFileHandle.Close()

		header, err := tar.FileInfoHeader(info, "")
		if err != nil {
			return errors.New("failed to create gzip file info header: " + err.Error())
		}
		header.Name = path

		tarWriter := tar.NewWriter(zipWriter)
		defer tarWriter.Close()

		if err := tarWriter.WriteHeader(header); err != nil {
			return errors.New("failed to write gzip header: " + err.Error())
		}

		if _, err := io.Copy(tarWriter, originFileHandle); err != nil {
			return errors.New("failed to copy zip to destination file: " + err.Error())
		}

		return nil
	})

	if err != nil {
		return errors.New("failed to walk source directory: " + err.Error())
	}

	return nil
}
