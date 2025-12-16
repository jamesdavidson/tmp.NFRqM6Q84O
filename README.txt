//Not sure why this does not work in Firefox. I should try Chrome.

var transport = new WebTransport('https://localhost:8080', {
   serverCertificateHashes: [
    {
      algorithm: "sha-256",
      value: Uint8Array.fromHex("ff2fdee0aa6d1044459cefa30da35fe9cb132f340286314fdf0f083cc75c15cd")
    }
  ]
});

