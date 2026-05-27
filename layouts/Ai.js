import { updateState } from "./state";

export async function runAI(prompt) {
  // replace with real API later
  return `AI → ${prompt}`;
}

export async function refreshAI(prompt) {
  const result = await runAI(prompt);
  updateState("ai.output", result);
}
