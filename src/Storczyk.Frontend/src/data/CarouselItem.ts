import type { Component } from "vue";

export type CarouselItem
  = CarouselComponentItem | CarouselImageItem;

export interface CarouselComponentItem {
  component: Component;
}
export interface CarouselImageItem {
  image_path: string;
  alt_text: string;
}
