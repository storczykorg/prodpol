<script setup lang="ts">

import {useI18n} from "vue-i18n";
import {watch} from "vue";

const { t, locale } = useI18n();

watch(locale, (newVal) => {
  localStorage.setItem("lang", newVal);
})
</script>

<template>
  <div class="drawer-side">
    <label for="my-drawer-5" aria-label="close sidebar" class="drawer-overlay"></label>
    <ul class="menu bg-base-200 min-h-full w-80 p-4">
      <!-- Sidebar content here -->
      <li><a>Sidebar Item 1</a></li>
      <li><router-link to="/admin">{{ t("admin.link") }}</router-link></li>
      <li class="mt-auto">
        <select v-model="locale" class="select select-sm w-full">
          <option disabled>Język</option>
          <option value="en">English</option>
          <option value="pl">Polski</option>
        </select>
      </li>
    </ul>
  </div>
</template>

<style scoped>

</style>