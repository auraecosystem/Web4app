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
