import { route, navigate } from "./kernel.js";
import * as React from "https://esm.unpkg.com/react@18.3.1";

/* Home */
route("/", () =>
  React.createElement("div", null,
    React.createElement("h1", null, "Web4 OS Kernel ⚡"),
    React.createElement("button", { onClick: () => navigate("/wallet") }, "Wallet"),
    React.createElement("button", { onClick: () => navigate("/ai") }, "AI")
  )
);

/* Wallet (Fadaka-ready hook) */
route("/wallet", () =>
  React.createElement("div", null,
    React.createElement("h1", null, "Wallet Module"),
    React.createElement("button", { onClick: () => navigate("/") }, "Back")
  )
);

/* AI route */
route("/ai", () =>
  React.createElement("div", null,
    React.createElement("h1", null, "AI Runtime"),
    React.createElement("button", { onClick: () => navigate("/") }, "Back")
  )
);

/* 404 */
route("/404", () =>
  React.createElement("h1", null, "404 - Route Missing")
);
