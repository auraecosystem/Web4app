export const XSIM = {
  execute(input) {
    let state = input.start || 0;
    const steps = [];

    for (let i = 0; i < 5; i++) {
      state = state * 1.15 + 2;
      steps.push(state);
    }

    return {
      final: state,
      steps
    };
  }
};
