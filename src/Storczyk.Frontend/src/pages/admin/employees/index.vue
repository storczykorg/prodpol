<script setup lang="ts">

import { useRouteQuery } from '@vueuse/router'
import {useI18n} from "vue-i18n";
import {ref} from "vue";

const { t } = useI18n()

const search_params = ref({
  id: useRouteQuery("id", ''),
  name: useRouteQuery("name", ''),
  group: useRouteQuery("group", 'all'),
  sort_key: useRouteQuery("sort_key", 'created_at'),
  sort: useRouteQuery("sort", 'asc'),
  limit: useRouteQuery("limit", '10'),
  page: useRouteQuery("page", '1'),
});

</script>

<template>
  <article class="items-center flex flex-col">
    <h2 class="font-bold text-xl">{{ t("admin.employees.link") }}</h2>
    <fieldset class="fieldset
    bg-base-200 border-base-300 rounded-box border
    p-4 flex flex-wrap justify-between w-full gap-8">
      <legend class="fieldset-legend">{{ t("ui.search.title") }}</legend>
      <div>
        <label class="label">Identyfikator</label>
        <input type="text" class="input" placeholder="My awesome page" v-model="search_params.id" />
      </div>
      <div>
        <label class="label">Nazwisko</label>
        <input type="text" class="input" placeholder="my-awesome-page" v-model="search_params.name" />
      </div>
      <div>
        <label class="label">Grupa</label>
        <select class="select select-neutral text-sm">
          <option selected>Wszyscy </option>
          <option>Magazyn</option>
          <option>Obsługa klienta</option>
          <option>Administracja</option>
          <option>Bez grupy</option>
        </select>
      </div>
    </fieldset>
    <fieldset class="fieldset
    bg-base-200 border-base-300 rounded-box border
    p-4 flex flex-wrap w-full justify-between gap-8">
      <legend class="fieldset-legend"> {{ t("ui.sorting.title") }} </legend>
      <div>
        <label class="label block">Sortowanie według</label>
        <select class="select select-neutral text-sm leading-0.5 w-lg">
          <option selected>Utworzono</option>
          <option>Imię i nazwisko</option>
          <option>Identyfikator</option>
          <option>Nazwa roli</option>
        </select>
      </div>
      <div>
        <label class="label">Kolejność</label>
        <select class="select select-neutral text-sm leading-0.5">
          <option selected>Malejąco</option>
          <option>Rosnąco</option>
        </select>
      </div>
    </fieldset>

    <div class="flex justify-between align-bottom flex-wrap w-full">
      <h3 class="text-lg font-semibold p-4">Znaleziono: 2137 wyników</h3>
      <fieldset class="fieldset">
        <legend class="fieldset-legend">Limit na stronę</legend>
        <select class="select select-neutral text-sm" v-model="search_params.limit">
          <option value="5">5</option>
          <option value="10">10</option>
          <option value="15">15</option>
          <option value="20">20</option>
          <option value="50">50</option>
          <option value="100">100</option>
        </select>
      </fieldset>
    </div>
    <div class="overflow-x-auto">
      <table class="table">
        <!-- head -->
        <thead>
        <tr>
          <th>
            <label>
              <input type="checkbox" class="checkbox" />
            </label>
          </th>
          <th>Name</th>
          <th>Job</th>
          <th>Favorite Color</th>
          <th></th>
        </tr>
        </thead>
        <tbody>
        <!-- row 1 -->
        <tr>
          <th>
            <label>
              <input type="checkbox" class="checkbox" />
            </label>
          </th>
          <td>
            <div class="flex items-center gap-3">
              <div class="avatar">
                <div class="mask mask-squircle h-12 w-12">
                  <img
                      src="https://img.daisyui.com/images/profile/demo/2@94.webp"
                      alt="Avatar Tailwind CSS Component" />
                </div>
              </div>
              <div>
                <div class="font-bold">Hart Hagerty</div>
                <div class="text-sm opacity-50">United States</div>
              </div>
            </div>
          </td>
          <td>
            Zemlak, Daniel and Leannon
            <br />
            <span class="badge badge-ghost badge-sm">Desktop Support Technician</span>
          </td>
          <td>Purple</td>
          <th>
            <button class="btn btn-ghost btn-xs">details</button>
          </th>
        </tr>
        <!-- row 2 -->
        <tr>
          <th>
            <label>
              <input type="checkbox" class="checkbox" />
            </label>
          </th>
          <td>
            <div class="flex items-center gap-3">
              <div class="avatar">
                <div class="mask mask-squircle h-12 w-12">
                  <img
                      src="https://img.daisyui.com/images/profile/demo/3@94.webp"
                      alt="Avatar Tailwind CSS Component" />
                </div>
              </div>
              <div>
                <div class="font-bold">Brice Swyre</div>
                <div class="text-sm opacity-50">China</div>
              </div>
            </div>
          </td>
          <td>
            Carroll Group
            <br />
            <span class="badge badge-ghost badge-sm">Tax Accountant</span>
          </td>
          <td>Red</td>
          <th>
            <button class="btn btn-ghost btn-xs">details</button>
          </th>
        </tr>
        <!-- row 3 -->
        <tr>
          <th>
            <label>
              <input type="checkbox" class="checkbox" />
            </label>
          </th>
          <td>
            <div class="flex items-center gap-3">
              <div class="avatar">
                <div class="mask mask-squircle h-12 w-12">
                  <img
                      src="https://img.daisyui.com/images/profile/demo/4@94.webp"
                      alt="Avatar Tailwind CSS Component" />
                </div>
              </div>
              <div>
                <div class="font-bold">Marjy Ferencz</div>
                <div class="text-sm opacity-50">Russia</div>
              </div>
            </div>
          </td>
          <td>
            Rowe-Schoen
            <br />
            <span class="badge badge-ghost badge-sm">Office Assistant I</span>
          </td>
          <td>Crimson</td>
          <th>
            <button class="btn btn-ghost btn-xs">details</button>
          </th>
        </tr>
        <!-- row 4 -->
        <tr>
          <th>
            <label>
              <input type="checkbox" class="checkbox" />
            </label>
          </th>
          <td>
            <div class="flex items-center gap-3">
              <div class="avatar">
                <div class="mask mask-squircle h-12 w-12">
                  <img
                      src="https://img.daisyui.com/images/profile/demo/5@94.webp"
                      alt="Avatar Tailwind CSS Component" />
                </div>
              </div>
              <div>
                <div class="font-bold">Yancy Tear</div>
                <div class="text-sm opacity-50">Brazil</div>
              </div>
            </div>
          </td>
          <td>
            Wyman-Ledner
            <br />
            <span class="badge badge-ghost badge-sm">Community Outreach Specialist</span>
          </td>
          <td>Indigo</td>
          <th>
            <button class="btn btn-ghost btn-xs">details</button>
          </th>
        </tr>
        </tbody>
        <!-- foot -->
        <tfoot>
        <tr>
          <th></th>
          <th>Name</th>
          <th>Job</th>
          <th>Favorite Color</th>
          <th></th>
        </tr>
        </tfoot>
      </table>
    </div>
    <div class="join">
      <button class="join-item btn">«</button>
      <button class="join-item btn">Page 22</button>
      <button class="join-item btn">»</button>
    </div>
  </article>
</template>

<style scoped>

</style>