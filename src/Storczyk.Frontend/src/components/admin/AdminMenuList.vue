<script setup lang="ts">
import type {MenuItem} from "../MenuItems.ts";

const props = defineProps<{
  className?: string,
  inNavbar?: boolean | "true" | "false",
  items: Array<MenuItem>
}>()
</script>

<template>
  <ul v-if="props.inNavbar" class="menu menu-horizontal px-1">
    <li v-for="(item, key) in props.items" :key="key">
      <router-link v-if="item.type == 'link'" :to="item.link">
        {{ item.text }}
      </router-link>
      <details v-else-if="item.type == 'group'">
        <summary>{{ item.text }}</summary>
        <ul class="p-2 bg-base-100 w-40 z-1">
          <li v-for="(subitem, subkey) in item.links" :key="subkey">
            <router-link :to="subitem.link">{{subitem.text}}</router-link>
          </li>
        </ul>
      </details>
    </li>
  </ul>
  <ul v-else class="menu">
    <li v-for="(item, key) in props.items" :key="key">
      <router-link v-if="item.type == 'link'" :to="item.link">
        {{ item.text }}
      </router-link>
      <template v-else-if="item.type == 'group'">
        <button>{{ item.text }}</button>
        <ul class="p-2 bg-base-100 w-40 z-1">
          <li v-for="(subitem, subkey) in item.links" :key="subkey">
            <router-link :to="subitem.link">{{subitem.text}}</router-link>
          </li>
        </ul>
      </template>
    </li>
  </ul>
</template>