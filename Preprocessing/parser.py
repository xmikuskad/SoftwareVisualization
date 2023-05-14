import json
import os
import re

# -----------------------------------------------------------------------


def get_json_string(json_filename):
    f = open("data/raw/"+json_filename, encoding="utf8")
    string = f.read()
    f.close()
    # print(string)
    # remove trailing zeros
    regex = r'''(?<=[}\]"']),(?!\s*[{["'])'''
    return re.sub(regex, "", string, 0)


def get_attributes_dict(js):
    attributes_dict = {}
    i = 0
    for attribute_type in js["attributeTypes"]:
        attributes_dict[i] = str(attribute_type["name"])
        # print("- attributeType " + str(i) + " = " + attributes_dict[i])
        i += 1
    return attributes_dict


def get_edge_archetypes_dict(js):
    edge_archetypes_dict = {}
    i = 0
    for edgeArchetype in js["edgeArchetypes"]:
        edge_archetypes_dict[i] = edgeArchetype["name"]
        # print("- edgeArchetype " + str(i) + " = " + edge_archetypes_dict[i])
        i += 1
    return edge_archetypes_dict


def get_vertex_archetypes_dict(js):
    vertex_archetypes_dict = {}
    i = 0
    for vertexArchetype in js["vertexArchetypes"]:
        vertex_archetypes_dict[i] = vertexArchetype["name"]
        # print("- vertexArchetype " + str(i) + " = " + vertex_archetypes_dict[i])
        i += 1
    return vertex_archetypes_dict


def rename_attributes(json, attributes_dict):
    for key_num in list(attributes_dict.keys()):
        if str(key_num) in json and key_num in attributes_dict:
            json[attributes_dict[key_num]] = json.pop(str(key_num))
    return json


# -----------------------------------------------------------------------

RAW_DATA_DIR = "data/raw/"
PARSED_DATA_DIR = "data/parsed/"

for json_filename in os.listdir(RAW_DATA_DIR):

    print("parsing " + json_filename + "...")

    js = json.loads(get_json_string(json_filename))

    attributes_dict = get_attributes_dict(js)
    edge_archetypes_dict = get_edge_archetypes_dict(js)
    vertex_archetypes_dict = get_vertex_archetypes_dict(js)

    for edge in js["edges"]:
        edge["archetype"] = edge_archetypes_dict[int(edge["archetype"])]
        edge["attributes"] = rename_attributes(edge["attributes"], attributes_dict)

    for vertex in js["vertices"]:
        vertex["archetype"] = vertex_archetypes_dict[int(vertex["archetype"])]
        vertex["attributes"] = rename_attributes(vertex["attributes"], attributes_dict)

    with open(PARSED_DATA_DIR+json_filename.replace(".json", "-parsed.json"), 'w', encoding="utf8") as f:
        json.dump(js, f, ensure_ascii=False, indent=4)
