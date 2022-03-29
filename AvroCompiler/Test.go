package Expedia

import (
	"encoding/base64"
	"errors"
	"time"

	"github.com/linkedin/goavro"
	"github.com/nats-io/nats.go"
)

func (request Request) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6IlJlcXVlc3QiLCJuYW1lc3BhY2UiOm51bGwsInN5bWJvbHMiOm51bGwsIm9yZGVyIjpudWxsLCJhbGlhc2VzIjpudWxsLCJzaXplIjpudWxsLCJmaWVsZHMiOlt7Im5hbWUiOiJMb2NhdGlvbiIsInR5cGUiOiJzdHJpbmciLCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJEYXRlQW5kVGltZSIsInR5cGUiOiJpbnQiLCJhbGlhc2VzIjpudWxsfV19"
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type Request struct {
	Location    string `avro:"Location"`
	DateAndTime int    `avro:"DateAndTime"`
}

func (response Response) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6IlJlc3BvbnNlIiwibmFtZXNwYWNlIjpudWxsLCJzeW1ib2xzIjpudWxsLCJvcmRlciI6bnVsbCwiYWxpYXNlcyI6bnVsbCwic2l6ZSI6bnVsbCwiZmllbGRzIjpbeyJuYW1lIjoicmVzdWx0cyIsInR5cGUiOiJzdHJpbmciLCJhbGlhc2VzIjpudWxsfV19"
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type Response struct {
	Results string `avro:"results"`
}

type GetResults func(request Request) (*Response, error)

func GetResultsClient(conn *nats.Conn) GetResults {

	requestCodec, requestCodecError := Request.Codec(Request{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	responseCodec, responseCodecError := Response.Codec(Response{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	return func(request Request) (*Response, error) {
		if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, request); requestEncodedError == nil {
			if response, error := conn.Request("$namespace", requestEncoded, time.Second*10); error == nil {
				if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
					if response, ok := responseDecoded.(Response); ok {
						return &response, nil
					} else {
						return nil, errors.New("Invalid message type")
					}
				} else {
					return nil, responseDecodedError
				}
			} else {
				return nil, error
			}
		} else {
			return nil, requestEncodedError
		}
	}

}

type A struct {
	Field [][][][][]int
}

func NewA() A {
	a := A{}

	return a
}
