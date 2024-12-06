package compactor

import (
	"errors"
	"fmt"
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

	return nil
}
