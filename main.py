from fastapi import FastAPI
import re

app = FastAPI(title="Web4 Runtime Core")

# ======================================================
# 🧩 WEB4 TWIG ENGINE
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
# 👛 WALLET LAYER
# ======================================================

class Wallet:
    def __init__(self):
        self.address = "0xWEB4_FADAKA_DEMO"

    def balance(self):
        return 1200

    def balance_of(self, token="FDAK"):
        return {
            "FDAK": 9999,
            "ETH": 2.5
        }.get(token, 0)


# ======================================================
# 🔗 CONTRACT LAYER
# ======================================================

class Contracts:
    class FDAK:
        def totalSupply(self):
            return "1,000,000,000"

        def price(self):
            return 0.42


# ======================================================
# 🤖 AI LAYER
# ======================================================

class AI:
    def generate(self, prompt=""):
        return f"AI Insight → {prompt or 'No prompt provided'}"

    def explain(self, data):
        return f"AI Explanation → {data}"


# ======================================================
# 🌐 CHAIN LAYER (simplified abstraction)
# ======================================================

class Chain:
    def __init__(self):
        self.fadaka = self

    def balance(self, address):
        return 8888

    def price(self, token):
        return 0.42


# ======================================================
# 🧠 CONTEXT BUILDER
# ======================================================

def build_context():
    return {
        "app": {
            "name": "Web4 Runtime Core"
        },
        "wallet": Wallet(),
        "chain": Chain(),
        "ai": AI(),
        "contract": Contracts()
    }


# ======================================================
# 🧩 TEMPLATE (embedded for simplicity)
# ======================================================

TEMPLATE = """
<h1>{{ app.name }}</h1>

<h2>Wallet</h2>
<p>Address: {{ wallet.address }}</p>
<p>Balance: {{ wallet.balance }}</p>
<p>FDAK Balance: {{ wallet.balance_of }}</p>

<h2>Chain</h2>
<p>FDAK Price: {{ chain.fadaka.price }}</p>

<h2>Contract</h2>
<p>Total Supply: {{ contract.FDAK.totalSupply }}</p>

<h2>AI Layer</h2>
<p>{{ ai.generate }}</p>
"""


# ======================================================
# 🚀 FASTAPI ENDPOINT
# ======================================================

@app.get("/")
def home():
    ctx = build_context()
    html = render(TEMPLATE, ctx)
    return {"rendered_html": html}


@app.get("/raw")
def raw():
    return build_context()
