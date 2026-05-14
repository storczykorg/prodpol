<script setup lang="ts">
import { GitHubIcon } from "vue3-simple-icons";
</script>

<template>
  <footer class="footer sm:footer-horizontal bg-neutral text-neutral-content items-center p-4">
    <aside class="grid-flow-col items-center">
      <img src="/public/logo64_placeholder.webp" height="24" width="32" />
      <p>Copyright © {{ new Date().getFullYear() }} - All right reserved</p>
    </aside>
    <nav class="grid-flow-col gap-4 md:place-self-center md:justify-self-end">
      <a target="_blank" href="https://github.com/storczykorg/prodpol">
        <GitHubIcon width="24" height="24" />
      </a>
    </nav>
  </footer>
</template>

<style scoped>

</style>