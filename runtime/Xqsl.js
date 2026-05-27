export const XQSL = {
  execute(input) {
    const facts = input.facts || [];

    const valid = facts.every(f => f === true);

    return {
      valid,
      confidence: valid ? 0.95 : 0.35
    };
  }
};
