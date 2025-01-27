package uploader

import (
	"errors"
	"fmt"
	"os"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/credentials"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/s3"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
)

type S3Client struct {
	bucketName string
	s3Client   *s3.S3
}

func Upload(compactedFile string) error {
	fmt.Println(
		"Uploading", compactedFile, "to storage",
	)

	s3Client, err := createS3Client()
	if err != nil {
		return errors.New("failed to create S3 client")
	}

	file, err := os.Open(compactedFile)
	if err != nil {
		return errors.New("failed to open file")
	}
	defer file.Close()

	uploader := s3manager.NewUploaderWithClient(s3Client.s3Client)

	_, err = uploader.Upload(&s3manager.UploadInput{
		Bucket: &s3Client.bucketName,
		Key:    &compactedFile,
		Body:   file,
	})
	if err != nil {
		return errors.New("failed to upload file")
	}

	return nil
}

func createS3Client() (*S3Client, error) {
	keyId := os.Getenv("BACKUP_S3_ACCESS_ID")
	applicationKey := os.Getenv("BACKUP_S3_SECRET_ID")
	bucketName := os.Getenv("BACKUP_S3_KEY_NAME")
	endpoint := os.Getenv("BACKUP_S3_ENDPOINT")
	region := os.Getenv("BACKUP_S3_REGION")

	s3Config := &aws.Config{
		Credentials: credentials.NewStaticCredentials(keyId, applicationKey, ""),
		Endpoint:    &endpoint,
		Region:      &region,
	}

	awsSession, err := session.NewSession(s3Config)
	if err != nil {
		return nil, errors.New("failed to create S3 session")
	}

	s3Client := s3.New(awsSession)

	return &S3Client{
		bucketName: bucketName,
		s3Client:   s3Client,
	}, nil
}
