import { Web4Runtime } from "./web4-runtime.js";
import { XAI } from "./xai.js";
import { XLSL } from "./xlsl.js";
import { XSIM } from "./xsim.js";
import { XQSL } from "./xqsl.js";

export const runtime = new Web4Runtime();

// register everything
runtime.register("ai", XAI);
runtime.register("xlsl", XLSL);
runtime.register("xsim", XSIM);
runtime.register("xqsl", XQSL);

// initial system state
runtime.set("app.name", "Web4 Unified Runtime");
runtime.set("system.status", "online");
