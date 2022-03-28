package Simple

import (
    "encoding/base64"

	"github.com/linkedin/goavro"
    "github.com/nats-io/nats.go"
)
        
type Kind int
const (
    FOO Kind = 0
    BAR Kind = 1
    BAZ Kind = 2
)

type MD5 string

func (testRecord TestRecord) Codec() (*goavro.Codec, error) {
    avroSchemaInBase64 := "{"type":"record","name":"TestRecord","symbols":null,"order":null,"aliases":null,"size":null,"fields":[{"name":"name","type":{"type":"string","order":"ignore"},"aliases":null},{"name":"kind","type":{"type":"enum","name":"Kind","symbols":["FOO","BAR","BAZ"],"order":"descending","aliases":["org.foo.KindOf"]},"aliases":null},{"name":"hash","type":{"type":"fixed","name":"MD5","size":16},"aliases":null},{"name":"nullableHash","type":["MD5","null"],"aliases":["hash"]},{"name":"arrayOfLongs","type":{"type":"array","items":"long"},"aliases":null}]}"
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
        
type TestRecord struct {
    Name string
    Kind Kind
    Hash MD5
    NullableHash *MD5
    ArrayOfLongs []int64
}


func (testError TestError) Codec() (*goavro.Codec, error) {
    avroSchemaInBase64 := "{
    "type" : "error",
    "name" : "TestError",
    "fields" : [ {
      "name" : "message",
      "type" : "string"
    } ]
  }"
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
        
type TestError struct {
    Message string
}



type Echo func(testRecord TestRecord) (*TestRecord, error)
func EchoClient(conn *nats.Conn) Echo {

requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
if requestCodecError != nil {
    panic(requestCodecError)
}


responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
if responseCodecError != nil {
    panic(responseCodecError)
}



return func(testRecord TestRecord) (*TestRecord, error) {
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
        if response, error := conn.Request("$namespace", requestEncoded, time.Second * 10); error == nil {
            if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
                if response, ok := responseDecoded.(TestRecord); ok {
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

type Echo2 func(testRecord TestRecord) (*TestRecord, *TestError, error)
func Echo2Client(conn *nats.Conn) Echo2 {

requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
if requestCodecError != nil {
    panic(requestCodecError)
}


responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
if responseCodecError != nil {
    panic(responseCodecError)
}


errorCodec, errorCodecError := TestError.Codec(TestError{})
if errorCodecError != nil {
    panic(errorCodec)
}


return func(testRecord TestRecord) (*TestRecord, *TestError, error) {
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
        if response, error := conn.Request("$namespace", requestEncoded, time.Second * 10); error == nil {
            if response.Header.Get("Status") == "200" {
                if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
                    if response, ok := responseDecoded.(TestRecord); ok {
                        return &response, nil, nil
                    } else {
                        return nil, nil, errors.New("Invalid message type")
                    }
                } else {
                    return nil, nil, responseDecodedError
                }
            } else {
                if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {
                    if error, ok := errorDecoded.(TestError); ok {
                        return nil, &error, nil
                    } else {
                        return nil, nil, errors.New("Invalid error type")
                    }
                } else {
                    return nil, nil, errorDecodedError
                }
            }

        } else {
            return nil, nil, error
        }
    } else {
        return nil, nil, requestEncodedError
    }
}

}

type Echo3 func(testRecord TestRecord) error
func Echo3Client(conn *nats.Conn) Echo3 {

requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
if requestCodecError != nil {
    panic(requestCodecError)
}



errorCodec, errorCodecError := TestError.Codec(TestError{})
if errorCodecError != nil {
    panic(errorCodec)
}


return func(testRecord TestRecord) error {
    if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
        return conn.Publish("$namespace", requestEncoded)
    } else {
        return requestEncodedError
    }
}

}

type Echo4 func() (*TestRecord, *TestError, error)
func Echo4Client(conn *nats.Conn) Echo4 {


responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
if responseCodecError != nil {
    panic(responseCodecError)
}


errorCodec, errorCodecError := TestError.Codec(TestError{})
if errorCodecError != nil {
    panic(errorCodec)
}


return func() (*TestRecord, *TestError, error) {
   if response, error := conn.Request("$namespace", []byte {}, time.Second * 10); error == nil {
        if response.Header.Get("Status") == "200" {
            if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
                if response, ok := responseDecoded.(TestRecord); ok {
                    return &response, nil, nil
                } else {
                    return nil, nil, errors.New("Invalid message type")
                }
            } else {
                return nil, nil, responseDecodedError
            }
        } else {
            if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {
                if error, ok := errorDecoded.(TestError); ok {
                    return nil, &error, nil
                } else {
                    return nil, nil, errors.New("Invalid error type")
                }
            } else {
                return nil, nil, errorDecodedError
            }
        }

    } else {
        return nil, nil, error
    }
}

}


type Echo5 func() error
func Echo5Client(conn *nats.Conn) Echo5 {




return func() error {
    return conn.Publish("$namespace", []byte {})
}

}


type Echo6 func() error
func Echo6Client(conn *nats.Conn) Echo6 {



errorCodec, errorCodecError := TestError.Codec(TestError{})
if errorCodecError != nil {
    panic(errorCodec)
}


return func() error {
    return conn.Publish("$namespace", []byte {})
}

}




type Error func() error
func ErrorClient(conn *nats.Conn) Error {



errorCodec, errorCodecError := TestError.Codec(TestError{})
if errorCodecError != nil {
    panic(errorCodec)
}


return func() error {
    return conn.Publish("$namespace", []byte {})
}

}


type Ping func() error
func PingClient(conn *nats.Conn) Ping {




return func() error {
    return conn.Publish("$namespace", []byte {})
}

}

