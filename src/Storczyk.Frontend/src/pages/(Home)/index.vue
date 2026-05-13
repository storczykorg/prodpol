<script lang="ts" setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

</script>

<template>
  <div
      class="hero min-h-1/4 rounded-2xl"
      style="background-image: url('/hero.webp');"
  >
    <div class="hero-overlay rounded-2xl"></div>
    <div class="hero-content text-neutral-content text-center rounded-2xl">
      <div class="max-w-md">
        <h1 class="mb-5 text-5xl font-bold">Prodpol</h1>
        <p class="mb-5">
          {{ t("index_hero") }}
        </p>
        <div class="flex gap-4 justify-center">
          <router-link to="/about" class="btn btn-primary">O nas</router-link>
          <router-link to="/work" class="btn btn-primary">Nasze portfolio</router-link>
        </div>
      </div>
    </div>
  </div>
  <article class="prose">

  </article>
</template>
<i18n lang="yaml">
en:
  index_hero: "We're company for you"
pl:
  index_hero: "Jesteśmy firmą w wieloletnim doświadczeniem w dziedzinie produkcji z użyciem metod numerycznych."
</i18n>