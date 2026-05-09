<script lang="ts" setup>

import { useI18n } from 'vue-i18n'
import NavCollapsable from "./components/NavCollapsable.vue";
import {BadgeInfoIcon, HomeIcon} from "@lucide/vue";
const { t } = useI18n()

const menuItems = [
  {
    icon: HomeIcon,
    href: "/",
    name: "site.homepage"
  },
  {
    icon: BadgeInfoIcon,
    href: "/about",
    name: "site.about"
  }
];

</script>

<template>
  <nav class="nav-collapsable-anchor">
    <ul>
      <li>
        <router-link to="/"> {{ t("site.homepage") }} </router-link>
      </li>
    </ul>
    <NavCollapsable v-bind:menuItems="menuItems"/>
  </nav>
  <main>
    <RouterView/>
  </main>
  <footer>
    {{ t("message.footer_text") }}
  </footer>
</template>