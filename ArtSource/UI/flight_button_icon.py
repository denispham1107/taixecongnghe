"""Flight button icons (21.140): a winged supercar seen from the side, with a chevron showing what a press does.
Run from the project root: python ArtSource/UI/flight_button_icon.py"""
from pathlib import Path
from PIL import Image, ImageDraw

S = 4
SIZE = 256 * S
OUT = Path('Assets/PhoViet/Resources/PlayerFlight')
BODY = (70, 170, 255, 255)
DARK = (10, 34, 58, 255)
WHITE = (255, 255, 255, 255)
GLASS = (190, 232, 255, 255)
GOLD = (255, 205, 80, 255)


def scaled(points):
    return [(x * S, y * S) for x, y in points]


def car(draw):
    # Low wedge-shaped supercar, nose to the right.
    outline = [(18, 190), (22, 172), (58, 162), (92, 138), (140, 134), (178, 156), (226, 166), (238, 182), (234, 196), (22, 198)]
    draw.polygon(scaled(outline), fill=BODY)
    draw.line(scaled(outline + [outline[0]]), fill=DARK, width=4 * S)
    draw.polygon(scaled([(94, 158), (106, 144), (136, 141), (160, 158)]), fill=GLASS)
    for cx in (66, 192):
        r = 19
        draw.ellipse(scaled([(cx - r, 194 - r), (cx + r, 194 + r)]), fill=DARK)
        r = 8
        draw.ellipse(scaled([(cx - r, 194 - r), (cx + r, 194 + r)]), fill=GLASS)
    # Large wing swept up and back, drawn over the body so it reads as a flying car.
    wing = [(126, 172), (24, 52), (58, 50), (92, 72), (168, 166)]
    draw.polygon(scaled(wing), fill=WHITE)
    draw.line(scaled(wing + [wing[0]]), fill=DARK, width=4 * S)
    draw.line(scaled([(44, 66), (138, 166)]), fill=GOLD, width=9 * S)


def chevron(draw, y, up, colour):
    t = -1 if up else 1
    draw.line(scaled([(146, y - t * 17), (182, y + t * 17), (218, y - t * 17)]), fill=colour, width=15 * S, joint='curve')


def icon(name, up):
    image = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    car(draw)
    if up:
        chevron(draw, 100, True, WHITE)
        chevron(draw, 62, True, GOLD)
    else:
        chevron(draw, 62, False, GOLD)
        chevron(draw, 100, False, WHITE)
    image = image.resize((256, 256), Image.LANCZOS)
    OUT.mkdir(parents=True, exist_ok=True)
    image.save(OUT / f'{name}.png')


icon('PV_FlightUp', True)
icon('PV_FlightLand', False)
print('ok')
