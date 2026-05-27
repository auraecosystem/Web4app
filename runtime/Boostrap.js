import { Web4OS } from "./os";
import { Web4AI } from "./ai";
import { XLSL } from "./xlsl";
import { XSIM } from "./xsim";
import { XQSL } from "./xqsl";

export const runtime = new Web4OS();

// register everything
runtime.register("ai", Web4AI);
runtime.register("xlsl", XLSL);
runtime.register("xsim", XSIM);
runtime.register("xqsl", XQSL);

// system init state
runtime.set("app.name", "Web4 OS v1");
runtime.set("system.status", "online");
