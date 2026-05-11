import { createApp } from "vue";
import { router } from "./routes";
import App from "./App.vue";
import { createPinia } from "pinia";
import { PiniaColada } from "@pinia/colada";
import { i18n } from "./i18n.ts";
import { PiniaColadaAutoRefetch } from "@pinia/colada-plugin-auto-refetch";

const pinia = createPinia();

createApp(App)
  .use(router)
  .use(pinia)
  .use(PiniaColada, {
    plugins: [
      PiniaColadaAutoRefetch({
        autoRefetch: false,
      }),
    ],
  })
  .use(i18n)
  .mount("#app");
