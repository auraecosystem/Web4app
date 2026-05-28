import re
import time
from dataclasses import dataclass
from copilot.session import PermissionRequestResult


# -----------------------------
# Risk scoring system
# -----------------------------
class RiskLevel:
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"


@dataclass
class PermissionDecision:
    allow: bool
    risk: str
    reason: str


# -----------------------------
# Core policy engine
# -----------------------------
class PermissionEngine:

    def __init__(self):
        self.audit_log = []

        self.dangerous_shell_patterns = [
            r"rm\s+-rf",
            r"mkfs",
            r":\(\)\s*{\s*:\|:\s*&\s*}",
            r"shutdown",
            r"dd\s+if=",
        ]

        self.allowed_shell_prefixes = [
            "git",
            "ls",
            "echo",
            "cat",
            "python",
            "node",
            "npm",
            "go",
        ]

    # -----------------------------
    # Risk evaluation
    # -----------------------------
    def evaluate_shell(self, cmd: str) -> PermissionDecision:
        cmd = cmd.strip()

        # HIGH risk patterns
        if any(re.search(p, cmd) for p in self.dangerous_shell_patterns):
            return PermissionDecision(False, RiskLevel.HIGH, "Dangerous system operation")

        # BLOCK unknown destructive commands
        if not any(cmd.startswith(p) for p in self.allowed_shell_prefixes):
            return PermissionDecision(False, RiskLevel.MEDIUM, "Command not in allowlist")

        return PermissionDecision(True, RiskLevel.LOW, "Safe shell command")

    def evaluate_write(self, file_name: str) -> PermissionDecision:
        if file_name is None:
            return PermissionDecision(False, RiskLevel.HIGH, "No file target")

        # Protect system-level paths
        if any(x in file_name for x in ["/etc", "/system", ".ssh", "id_rsa"]):
            return PermissionDecision(False, RiskLevel.HIGH, "Protected system path")

        # Smart allow for your project zones
        if any(x in file_name for x in ["contracts", "fadaka", "web4", "src", "api"]):
            return PermissionDecision(True, RiskLevel.MEDIUM, "Project workspace file")

        return PermissionDecision(True, RiskLevel.LOW, "Normal file operation")

    def evaluate_url(self, url: str) -> PermissionDecision:
        if not url:
            return PermissionDecision(False, RiskLevel.HIGH, "Empty URL")

        # Block obvious risky schemes
        if any(url.startswith(x) for x in ["file://", "ftp://"]):
            return PermissionDecision(False, RiskLevel.HIGH, "Unsafe protocol")

        return PermissionDecision(True, RiskLevel.LOW, "Safe URL fetch")

    # -----------------------------
    # Logging
    # -----------------------------
    def log(self, request, decision: PermissionDecision):
        self.audit_log.append({
            "time": time.time(),
            "kind": request.kind.value,
            "tool": getattr(request, "tool_name", None),
            "file": getattr(request, "file_name", None),
            "command": getattr(request, "full_command_text", None),
            "decision": decision.__dict__,
        })


# -----------------------------
# Global engine instance
# -----------------------------
engine = PermissionEngine()


# -----------------------------
# Hook
# -----------------------------
def on_permission_request(request, invocation):
    kind = request.kind.value

    decision = None

    # SHELL
    if kind == "shell":
        decision = engine.evaluate_shell(request.full_command_text or "")

    # WRITE
    elif kind == "write":
        decision = engine.evaluate_write(request.file_name)

    # READ (light protection only)
    elif kind == "read":
        decision = PermissionDecision(True, RiskLevel.LOW, "Read allowed")

    # URL FETCH
    elif kind == "url":
        decision = engine.evaluate_url(getattr(request, "url", ""))

    # MCP / custom tools (treat as medium risk by default)
    elif kind in ["mcp", "custom-tool"]:
        decision = PermissionDecision(True, RiskLevel.MEDIUM, "External tool execution")

    # MEMORY ACCESS (important for AI safety)
    elif kind == "memory":
        decision = PermissionDecision(True, RiskLevel.MEDIUM, "Memory access granted")

    else:
        decision = PermissionDecision(False, RiskLevel.HIGH, "Unknown request type")

    # log everything
    engine.log(request, decision)

    # final decision mapping
    if not decision.allow:
        return PermissionRequestResult(kind="denied-interactively-by-user")

    return PermissionRequestResult(kind="approved")
