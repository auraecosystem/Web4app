import franken from "franken-ui/plugin-vite";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [
    franken({
      preflight: true,
    }),
  ],
});
