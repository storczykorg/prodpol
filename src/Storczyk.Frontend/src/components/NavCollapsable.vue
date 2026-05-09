<script lang="ts" setup>

import {HomeIcon, Menu} from "@lucide/vue";
import {useI18n} from "vue-i18n";
import type {MenuItem} from "../data/MenuItem.ts";

const {t} = useI18n();

const { menuItems } = defineProps<{ menuItems: MenuItem[] }>();


const addMenuItems = [
  {
    icon: HomeIcon,
    href: "/",
    name: "site.homepage"
  }
]

</script>

<template>
    <button popovertarget="nav-collapsable"> <Menu/> </button>

  <div id="nav-collapsable" class="nav-collapsable" popover>
    <ul class="max-w-60">
        <li v-for="(value, key) in menuItems" :key="key">
          <router-link :to="{path: value.href}" class="flex w-full content-between justify-between">
          <span class="inline-block">
          <component :is="value.icon"/>

          </span>
            <span class="inline-block">
          {{ t( value.name ) }}
          </span>
          </router-link>
        </li>
    </ul>
    <hr class="w-full m-auto" />
    <ul class="max-w-60">
      <li v-for="(value, key) in addMenuItems" :key="key">
        <router-link :to="{path: value.href}" class="inline-flex w-full justify-between">
          <span>
          <component :is="value.icon"/>

          </span>
          <span>
          {{ t( value.name ) }}
          </span>
        </router-link>
      </li>
    </ul>
  </div>
</template>

<style scoped>

</style>