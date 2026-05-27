export const XAI = {
  init(runtime) {
    runtime.ai = async (prompt) => {
      return `AI RESPONSE → ${prompt}`;
    };
  },

  execute(input, runtime) {
    return runtime.ai(input.prompt);
  }
};
