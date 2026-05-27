export const AIPlugin = {
  init(web4) {
    web4.ai = async function (prompt) {
      // replace later with OpenAI / local model
      return `AI RESPONSE → ${prompt}`;
    };
  }
};
