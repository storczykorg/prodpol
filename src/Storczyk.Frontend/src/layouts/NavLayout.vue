<script setup lang="ts">
import { Ellipsis } from '@lucide/vue'

import DrawerSide from "../components/nav/drawerSide.vue";
import NavFooter from "../components/nav/navFooter.vue";

</script>

<template>
  <nav class="navbar sticky flex align-middle justify-center glass z-1">
    <span class="navbar-start"></span>
    <span class="navbar-center">
        <router-link class="font-bold text-lg" to="/">Prodpol</router-link>
    </span>
    <span class="navbar-end">
      <div class="drawer drawer-end">
      <input id="my-drawer-5" type="checkbox" class="drawer-toggle" />
      <div class="drawer-content justify-end text-end block">
        <!-- Page content here -->
        <label for="my-drawer-5" class="drawer-button btn btn-ghost">
          <Ellipsis />
        </label>
      </div>
        <drawer-side/>
      </div>
    </span>
  </nav>
  <main class="py-4 grid place-items-center">
    <slot></slot>
  </main>
  <nav-footer/>
</template>