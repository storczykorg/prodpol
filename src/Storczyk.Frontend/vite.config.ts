import { defineConfig } from "vite";
import VueRouter from "vue-router/vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import VueI18nPlugin from "@intlify/unplugin-vue-i18n/vite";
import path from "path";

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
  ],
  build: {
    cssCodeSplit: true,
    license: true,
    target: "baseline-widely-available",
  },
});
