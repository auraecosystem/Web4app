```txt id="h0o67h"
spacegramm/
├── server/
│   ├── package.json
│   ├── tsconfig.json
│   └── src/
│       └── index.ts
│
└── client/
    ├── index.html
    ├── app.js
    └── style.css
```

# server/package.json

```json id="z3ls3d"
{
  "name": "spacegramm-server",
  "version": "0.1.0",
  "type": "module",
  "scripts": {
    "dev": "tsx watch src/index.ts"
  },
  "dependencies": {
    "fastify": "^4.28.1",
    "jsonwebtoken": "^9.0.2",
    "ws": "^8.18.0"
  },
  "devDependencies": {
    "@types/jsonwebtoken": "^9.0.6",
    "@types/ws": "^8.5.12",
    "tsx": "^4.19.1",
    "typescript": "^5.6.3"
  }
}
```

# server/tsconfig.json

```json id="l5y6r3"
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ES2022",
    "moduleResolution": "Bundler",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true
  }
}
```

# server/src/index.ts

```ts id="z88m2j"
import Fastify from "fastify";
import { WebSocketServer } from "ws";
import jwt from "jsonwebtoken";
import http from "node:http";

const app = Fastify({
  logger: true,
});

const SECRET = "spacegramm-dev-secret";

interface Client {
  id: string;
  socket: WebSocket;
  username: string;
}

const messages: any[] = [];

const server = http.createServer(app.server);

const wss = new WebSocketServer({
  server,
});

const clients = new Map<string, Client>();

app.get("/", async () => {
  return {
    name: "Spacegramm Gateway",
    status: "online",
  };
});

app.post("/auth/login", async (request: any) => {
  const { username } = request.body;

  if (!username) {
    return {
      error: "Username required",
    };
  }

  const token = jwt.sign(
    {
      username,
    },
    SECRET,
    {
      expiresIn: "7d",
    }
  );

  return {
    token,
  };
});

wss.on("connection", (socket, request) => {
  try {
    const url = new URL(request.url!, "http://localhost");

    const token = url.searchParams.get("token");

    if (!token) {
      socket.close();
      return;
    }

    const payload = jwt.verify(token, SECRET) as any;

    const clientId = crypto.randomUUID();

    const client: Client = {
      id: clientId,
      socket,
      username: payload.username,
    };

    clients.set(clientId, client);

    console.log(
      `[Spacegramm] ${client.username} connected`
    );

    socket.send(
      JSON.stringify({
        event: "system.connected",
        payload: {
          id: clientId,
          username: client.username,
        },
      })
    );

    socket.send(
      JSON.stringify({
        event: "message.history",
        payload: messages,
      })
    );

    socket.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data.toString());

        if (data.event === "message.send") {
          const message = {
            id: crypto.randomUUID(),
            username: client.username,
            content: data.payload.content,
            timestamp: Date.now(),
          };

          messages.push(message);

          const encoded = JSON.stringify({
            event: "message.new",
            payload: message,
          });

          for (const [, connectedClient] of clients) {
            connectedClient.socket.send(encoded);
          }
        }
      } catch (error) {
        console.error(error);
      }
    };

    socket.onclose = () => {
      clients.delete(clientId);

      console.log(
        `[Spacegramm] ${client.username} disconnected`
      );
    };
  } catch (error) {
    console.error(error);
    socket.close();
  }
});

server.listen(3000, () => {
  console.log(
    "[Spacegramm] Gateway running on http://localhost:3000"
  );
});
```

# client/index.html

```html id="j8m6dd"
<!DOCTYPE html>
<html>
<head>
  <title>Spacegramm</title>
  <link rel="stylesheet" href="./style.css">
</head>
<body>

<div id="app">
  <div class="header">
    Spacegramm
  </div>

  <div id="messages"></div>

  <div class="composer">
    <input id="messageInput" placeholder="Message Spacegramm..." />
    <button id="sendButton">
      Send
    </button>
  </div>
</div>

<script src="./app.js"></script>
</body>
</html>
```

# client/style.css

```css id="r8tm29"
body {
  margin: 0;
  background: #0f0f10;
  color: white;
  font-family: sans-serif;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.header {
  padding: 16px;
  background: #16181c;
  font-size: 20px;
  font-weight: bold;
}

#messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.message {
  margin-bottom: 12px;
}

.username {
  color: #7aa2ff;
  font-weight: bold;
}

.composer {
  display: flex;
  padding: 16px;
  background: #16181c;
}

#messageInput {
  flex: 1;
  padding: 12px;
  background: #22252b;
  border: none;
  color: white;
}

#sendButton {
  margin-left: 12px;
  padding: 12px 18px;
}
```

# client/app.js

```js id="r36r3n"
const messagesElement =
  document.getElementById("messages");

const input =
  document.getElementById("messageInput");

const button =
  document.getElementById("sendButton");

async function start() {
  const username =
    prompt("Enter username");

  const response = await fetch(
    "http://localhost:3000/auth/login",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        username,
      }),
    }
  );

  const auth = await response.json();

  const socket = new WebSocket(
    `ws://localhost:3000?token=${auth.token}`
  );

  socket.onmessage = (event) => {
    const data = JSON.parse(event.data);

    if (data.event === "message.history") {
      data.payload.forEach(renderMessage);
    }

    if (data.event === "message.new") {
      renderMessage(data.payload);
    }
  };

  button.onclick = () => {
    const content = input.value;

    if (!content) return;

    socket.send(
      JSON.stringify({
        event: "message.send",
        payload: {
          content,
        },
      })
    );

    input.value = "";
  };
}

function renderMessage(message) {
  const div = document.createElement("div");

  div.className = "message";

  div.innerHTML = `
    <span class="username">
      ${message.username}
    </span>
    ${message.content}
  `;

  messagesElement.appendChild(div);

  messagesElement.scrollTop =
    messagesElement.scrollHeight;
}

start();
```

# Run

## Server

```bash id="3tjlwm"
cd server
npm install
npm run dev
```

## Client

Open:

```text id="dfk8tl"
client/index.html
```

in browser.

```
```
