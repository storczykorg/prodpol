<script lang="ts" setup>

import 'vue3-carousel/carousel.css'
import { Carousel, Slide, Navigation, Pagination, CarouselConfig } from "vue3-carousel";
import { useI18n } from "vue-i18n";

const t = useI18n();

const carouselConfig = {
  itemsToShow: 2.5,
  wrapAround: true,
  gap: 4,
  enabled: true,
} as CarouselConfig;

</script>

<template>
  <div class="site-index card">
    <div class="card-header">Lorem</div>
    <div class="card-content"> Lorem ipsum </div>
    <carousel v-bind="carouselConfig">
      <slide class="carousel-item">
        <img src="https://picsum.photos/id/0/5000/3333">
      </slide>
      <slide class="carousel-item">
        <img src="https://picsum.photos/id/3/5000/3333">
      </slide>
      <slide class="carousel-item">
        <img src="https://picsum.photos/id/5/5000/3333">
      </slide>
      <slide class="carousel-item">
        <img src="https://picsum.photos/id/8/5000/3333">
      </slide>
      <template #addons>
        <Navigation />
        <Pagination />
      </template>
    </carousel>
  </div>
</template>

<style>
@reference "tailwindcss";

.carousel-item {
    width: 100%;
    height: 30vh;
  img {
    width: 100%;
    height: 100%;
    resize: both;
    object-fit: cover;
    object-position: center;
  }
}
</style>