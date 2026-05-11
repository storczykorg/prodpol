export type MenuItem = MenuLinkType | MenuGroup;
export type MenuLinkType = {
  type: "link";
  link: string;
  text: string;
};
export type MenuGroup = {
  type: "group";
  text: string;
  links: MenuLinkType[];
};
