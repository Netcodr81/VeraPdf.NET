/*
 * This file is part of veraPDF Feature Reporting, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Feature Reporting is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Feature Reporting as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Feature Reporting as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.features.gf.objects;

import org.verapdf.cos.COSKey;
import org.verapdf.cos.COSObject;
import org.verapdf.features.objects.OutlinesFeaturesObjectAdapter;
import org.verapdf.pd.PDOutlineDictionary;
import org.verapdf.pd.PDOutlineItem;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Feature object adapter for outlines
 *
 * <p>Adapts the document's bookmark tree to the reporter. Each outline
 * item provides its title, color, style and optional indirect-object number,
 * and exposes nested items as children.</p>
 *
 * <p><b>.NET port:</b> Port the outline and feature-adapter contracts where
 * no matching parser API exists. Use {@code List<T>} and a read-only view
 * for the returned children, and {@code int?} for an absent object number.
 * Preserve PDF sibling order and null checks when walking the tree.</p>
 *
 * @author Maksim Bezrukov
 */
public class GFOutlinesFeaturesObjectAdapter implements OutlinesFeaturesObjectAdapter {

	private final PDOutlineDictionary outline;

	/**
	 * Constructs new OutputIntent Feature Object adapter
	 *
	 * @param outline class represents outlines object
	 */
	public GFOutlinesFeaturesObjectAdapter(PDOutlineDictionary outline) {
		this.outline = outline;
	}

	@Override
	public List<OutlineFeaturesObjectAdapter> getChildren() {
		return GFOutlineFeaturesObjectAdapter.getChildren(outline);
	}

	@Override
	public boolean isPDFObjectPresent() {
		return outline != null && !outline.empty();
	}

	@Override
	public List<String> getErrors() {
		return Collections.emptyList();
	}

	/** Adapts one bookmark; the same helper walks either a root or an item. */
	private static class GFOutlineFeaturesObjectAdapter implements OutlineFeaturesObjectAdapter {

		private final PDOutlineItem outline;

		public GFOutlineFeaturesObjectAdapter(PDOutlineItem outline) {
			this.outline = outline;
		}

		@Override
		public Integer getKeyNumber() {
			if (outline != null && !outline.empty()) {
				COSObject cosObject = outline.getObject();
				COSKey objectKey = cosObject.getObjectKey();
				if (objectKey != null) {
					return objectKey.getNumber();
				}
			}
			return null;
		}

		@Override
		public String getTitle() {
			if (outline != null && !outline.empty()) {
				return outline.getTitle();
			}
			return null;
		}

		@Override
		public double[] getColor() {
			if (outline != null && !outline.empty()) {
				return outline.getColor();
			}
			return null;
		}

		@Override
		public boolean isItalic() {
			return outline != null && !outline.empty() && outline.isItalic();
		}

		@Override
		public boolean isBold() {
			return outline != null && !outline.empty() && outline.isBold();
		}

		@Override
		public List<OutlineFeaturesObjectAdapter> getChildren() {
			return getChildren(outline);
		}

		private static List<OutlineFeaturesObjectAdapter> getChildren(PDOutlineDictionary dict) {
			if (dict != null && !dict.empty()) {
				List<PDOutlineItem> children = getPDChildren(dict);
				List<OutlineFeaturesObjectAdapter> res = new ArrayList<>();
				for (PDOutlineItem item : children) {
					if (item != null) {
						res.add(new GFOutlineFeaturesObjectAdapter(item));
					}
				}
				return Collections.unmodifiableList(res);
			}
			return Collections.emptyList();
		}

		/** Follow First and Next links in their PDF order, rather than recursing into descendants. */
		private static List<PDOutlineItem> getPDChildren(PDOutlineDictionary dictionary) {
			List<PDOutlineItem> res = new ArrayList<>();
			PDOutlineItem curr = dictionary.getFirst();
			while (curr != null) {
				res.add(curr);
				curr = curr.getNext();
			}
			return res;
		}
	}
}
