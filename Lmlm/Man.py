from fastapi import FastAPI
from pydantic import BaseModel
import re

app = FastAPI(title="Web4 Twig Runtime")

# ======================================================
# 🧩 TWIG ENGINE
# ======================================================

VAR_PATTERN = re.compile(r"\{\{\s*(.*?)\s*\}\}")

def resolve(expr, ctx):
    try:
        parts = expr.split(".")
        value = ctx[parts[0]]

        for p in parts[1:]:
            value = getattr(value, p, None) if hasattr(value, p) else value[p]

        return str(value)
    except:
        return f"[undefined:{expr}]"


def render(template: str, ctx: dict):
    def repl(match):
        return resolve(match.group(1), ctx)

    return VAR_PATTERN.sub(repl, template)


# ======================================================
# 🤖 AI LAYER (mock but extensible)
# ======================================================

class AI:
    def generate(self, prompt=""):
        return f"AI Response → {prompt or 'system active'}"

# ======================================================
# 🌐 CONTEXT ENGINE
# ======================================================

def build_context():
    return {
        "app": {
            "name": "Web4 Twig Runtime"
        },
        "user": {
            "name": "Guest User",
            "status": "active"
        },
        "data": {
            "metricA": 42,
            "metricB": 99
        },
        "ai": AI(),
        "event": {
            "message": "System initialized successfully"
        }
    }


# ======================================================
# 🧩 WEB4 TWIG TEMPLATE
# ======================================================

TEMPLATE = """
<html>
<head>
  <title>{{ app.name }}</title>
</head>

<body>

<h1>{{ app.name }}</h1>

<section>
  <h2>User</h2>
  <p>Name: {{ user.name }}</p>
  <p>Status: {{ user.status }}</p>
</section>

<section>
  <h2>Data Layer</h2>
  <p>Metric A: {{ data.metricA }}</p>
  <p>Metric B: {{ data.metricB }}</p>
</section>

<section>
  <h2>AI Layer</h2>
  <p>{{ ai.generate }}</p>
</section>

<section>
  <h2>Event System</h2>
  <p>{{ event.message }}</p>
</section>

</body>
</html>
"""


# ======================================================
# 🚀 REQUEST MODELS
# ======================================================

class RenderRequest(BaseModel):
    user_name: str | None = "Guest User"
    status: str | None = "active"


# ======================================================
# 🚀 API ENDPOINTS
# ======================================================

@app.get("/")
def home():
    ctx = build_context()
    html = render(TEMPLATE, ctx)
    return {"html": html}


@app.post("/render")
def render_dynamic(req: RenderRequest):
    ctx = build_context()
    ctx["user"]["name"] = req.user_name
    ctx["user"]["status"] = req.status

    html = render(TEMPLATE, ctx)

    return {
        "rendered": html,
        "context": ctx
    }


@app.get("/event")
def event_stream():
    return {
        "event": "update",
        "message": "Live system heartbeat OK"
    }
