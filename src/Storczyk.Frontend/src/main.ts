import { createApp } from "vue";
import { router } from "./routes";
import "./style.css";
import App from "./App.vue";
import { createPinia } from "pinia";
import { PiniaColada } from "@pinia/colada";
import { i18n } from "./i18n.ts";

const pinia = createPinia();

createApp(App)
  .use(router)
  .use(pinia)
  .use(PiniaColada, {})
  .use(i18n)
  .mount("#app");
