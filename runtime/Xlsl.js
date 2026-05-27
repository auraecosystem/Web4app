export const XLSL = {
  execute(input) {
    const data = input.data || [];
    const sum = data.reduce((a, b) => a + b, 0);

    return {
      rows: data.length,
      sum,
      avg: data.length ? sum / data.length : 0
    };
  }
};
