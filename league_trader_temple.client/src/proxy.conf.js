const { env } = require('process');

function firstDefinedUrl() {
  if (env.ASPNETCORE_URLS) {
    const urls = env.ASPNETCORE_URLS.split(';');
    return urls.find(url => url.startsWith('https://')) ?? urls[0];
  }

  const httpsPort = env.ASPNETCORE_HTTPS_PORT ?? env.ASPNETCORE_HTTPS_PORTS?.split(';')[0];
  if (httpsPort) {
    return `https://localhost:${httpsPort}`;
  }

  const httpPort = env.ASPNETCORE_HTTP_PORT ?? env.ASPNETCORE_HTTP_PORTS?.split(';')[0];
  if (httpPort) {
    return `http://localhost:${httpPort}`;
  }

  return 'http://localhost:5248';
}

const target = firstDefinedUrl();

const PROXY_CONFIG = [
  {
    context: [
      "/riftboundcards",
      "/Account/login",
      "/Account/register"
    ],
    target,
    secure: false,
    changeOrigin: true
  }
]

module.exports = PROXY_CONFIG;
