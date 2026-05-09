<script setup lang="ts">

</script>

<template>
  <nav class="navbar sticky flex justify-between align-middle glass">
    <span>

    </span>
    <span>
      <router-link class="font-bold text-lg" to="/">Prodpol</router-link>
    </span>
    <span>

    </span>
  </nav>
  <main class="p-4">
    <slot></slot>
  </main>
  <footer></footer>
</template>