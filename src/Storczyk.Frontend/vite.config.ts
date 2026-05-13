import { defineConfig } from "vite";
import VueRouter from "vue-router/vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import VueI18nPlugin from "@intlify/unplugin-vue-i18n/vite";
import path from "path";

import * as child from "child_process";
import virtual from "@rollup/plugin-virtual";

const commitInfo = child.execSync("git log -1 HEAD").toString();

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    VueRouter({
    }),
    vue(),
    tailwindcss({
      optimize: true,
    }),
    VueI18nPlugin({
      include: [path.resolve(__dirname, "./src/locales/**")],
    }),
    virtual({
      "virtual:commitInfo": `const commitInfo = ${JSON.stringify(commitInfo)}; export default commitInfo;`,
    }),
  ],
  build: {
    cssCodeSplit: true,
    license: true,
    target: "baseline-widely-available",
  },
  server: {
    proxy: {
      "/api": {
        target: "http://127.0.0.1:8000/",
      },
    },
  },
});
