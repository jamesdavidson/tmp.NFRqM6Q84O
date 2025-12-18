//Not sure why this does not work in Firefox. I should try Chrome.

var transport = new WebTransport('https://localhost:8080', {
   serverCertificateHashes: [
    {
      algorithm: "sha-256",
      value: Uint8Array.fromHex("ff2fdee0aa6d1044459cefa30da35fe9cb132f340286314fdf0f083cc75c15cd")
    }
  ]
});

// flupke works great with RSA:

openssl req -newkey rsa:2048 -nodes -keyout certificate.key -x509 -out certificate.pem -subj '/CN=WebTransportTest1' -addext "subjectAltName = DNS:localhost"

// and flupke should work with ECDSA if I adapt sample to pass ecCurve="secp256r1" to .withKeyStore(KeyStore keyStore, String certificateAlias, char[] privateKeyPassword, String ecCurve)

openssl req -new -x509 -nodes out cert.pem -keyout key.pem -newkey ec -pkeyopt ec_paramgen_curve:prime256v1 -subj '/CN=localhost' -days 14 -addext "subjectAltName = DNS:localhost"

openssl x509 -pubkey -noout -in cert.pem | openssl pkey -pubin -outform der | openssl dgst -sha256 -binary | openssl enc -base64

var wt = new WebTransport('https://localhost:4433/echo', {
  serverCertificateHashes: [
    algorithm: "sha-256",
    value:  Uint8Array.fromBase64("Uj8/P2Q/Pz9cUT8/BT8/ID8EXE0/Pz8/Bj8/BFs/PnUNCg==")
  ]
});
